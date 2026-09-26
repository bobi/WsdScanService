#!/bin/sh
# Autocrop converter: autocrop.sh INPUT OUTPUT [magick output options...]
# Crops the scan to the detected objects. Objects are found by local edges (ink and physical outlines of
# cards/paper) on a 640px-wide copy (1 mask px ~ 0.34mm of bed at any dpi), so smooth lid shading is ignored.
# Scanner bed artifacts (dark strip, lid streaks) are lines within BAND px of a side with edges on >= 10% of it;
# each side is shaved by its own artifact width. A side is kept at the bed edge only when an object touches it
# (within EDGE px past the shaved band); other sides are cropped and padded with white.
# Tunables (env): AUTOCROP_THRESH, AUTOCROP_BAND, AUTOCROP_EDGE, AUTOCROP_PAD, AUTOCROP_DEBUG=1 (prints crop to stderr)
set -eu

in=$1
out=$2
shift 2

MASKW=640
THRESH=${AUTOCROP_THRESH:-3%} # min local brightness step counted as an edge
BAND=${AUTOCROP_BAND:-10}     # mask px from each bed edge searched for artifacts (~3.4mm)
EDGE=${AUTOCROP_EDGE:-3}      # mask px past the shaved band counted as "touching" the bed edge (~1mm)
PAD=${AUTOCROP_PAD:-0.01}     # white padding on cropped sides, fraction of cropped width/height

tmp=$(mktemp)
trap 'rm -f "$tmp"' EXIT

# Edge map; prints full-res size "FW FH".
fsize=$(magick "$in" -set option:fw '%w' -set option:fh '%h' \
    -colorspace Gray -resize "${MASKW}x" -blur 0x1 -morphology Edge Diamond:1 -threshold "$THRESH" \
    -write "PNG:$tmp" -format '%[fw] %[fh]' info:)

# Edge coverage of each of the first BAND lines parallel to each side, rows: left, right, top, bottom.
# Shave each side up to its last line that is >= 10% covered (objects rarely sit within BAND of the bed edge).
eval "$(magick "PNG:$tmp" \
    \( -clone 0 -crop "${BAND}x+0+0" \) \( -clone 0 -flop -crop "${BAND}x+0+0" \) \
    \( -clone 0 -rotate -90 -crop "${BAND}x+0+0" \) \( -clone 0 -rotate 90 -crop "${BAND}x+0+0" \) \
    -delete 0 +repage -scale "${BAND}x1!" -append -depth 16 txt:- | awk -F'[,:() ]+' '
    NR>1 && $3/65535>=0.10 { last[$2]=$1+1 }
    END { split("sl sr st sb", n, " "); for (i=0;i<4;i++) printf "%s=%d\n", n[i+1], last[i] }')"

# "w h x y W H": object bbox and mask size.
bbox=$(magick "PNG:$tmp" -background black \
    -gravity West -chop "${sl}x0" -splice "${sl}x0" -gravity East -chop "${sr}x0" -splice "${sr}x0" \
    -gravity North -chop "0x${st}" -splice "0x${st}" -gravity South -chop "0x${sb}" -splice "0x${sb}" +gravity \
    -morphology Close Disk:4 \
    -define connected-components:area-threshold=150 -define connected-components:mean-color=true \
    -connected-components 8 -trim \
    -format '%w %h %[fx:page.x] %[fx:page.y] %W %H' info:)

eval "$(echo "$bbox $fsize $sl $sr $st $sb" | awk -v e="$EDGE" -v p="$PAD" '{
    w=$1; h=$2; x=$3; y=$4; W=$5; H=$6; fw=$7; fh=$8
    sx=fw/W; sy=fh/H
    l=(x<=$9+e)?0:int(x*sx); t=(y<=$11+e)?0:int(y*sy)
    r=(W-x-w<=$10+e)?fw:int((x+w)*sx+0.999); b=(H-y-h<=$12+e)?fh:int((y+h)*sy+0.999)
    if (r>fw) r=fw; if (b>fh) b=fh
    cw=r-l; ch=b-t; px=int(cw*p+0.5); py=int(ch*p+0.5)
    printf "crop=%dx%d+%d+%d\n", cw, ch, l, t
    printf "padtl=%dx%d\n", (l>0?px:0), (t>0?py:0)
    printf "padbr=%dx%d\n", (r<fw?px:0), (b<fh?py:0)
}')"

if [ -n "${AUTOCROP_DEBUG:-}" ]; then
    echo "shave L$sl R$sr T$st B$sb mask $bbox -> crop $crop pad $padtl / $padbr" >&2
fi

magick "$in" -crop "$crop" +repage -background white \
    -gravity NorthWest -splice "$padtl" -gravity SouthEast -splice "$padbr" +gravity \
    "$@" "$out"
