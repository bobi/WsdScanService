using System.CodeDom.Compiler;
using System.Text;
using YamlDotNet.Serialization;

namespace WsdScanService.Common.Extensions;

public static class ObjectExtensions
{
    public static string DumpAsYaml(this object o)
    {
        var stringBuilder = new StringBuilder();

        var serializer = new Serializer();

        using var indentedTextWriter = new IndentedTextWriter(new StringWriter(stringBuilder));
        
        serializer.Serialize(indentedTextWriter, o);

        return stringBuilder.ToString();
    }
}