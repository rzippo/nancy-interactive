using System.Text;
using Giraffe.ViewEngine;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.DotNet.Interactive.Http;
using Microsoft.FSharp.Collections;
using XPlot.Plotly;

namespace Unipi.Nancy.Interactive;

/// <summary>
/// This class actually ports and exposes method from the XPlot.Plotly.Interactive package
/// which are for some reason NOT EXPOSED.
/// It seems, for some reason, to be the only code that will consistently render in Notebooks.
/// </summary>
public static class PlotlyExtensions
{
    public static string GetScriptElementWithRequire(string script)
    {
        var newScript = new StringBuilder();
        newScript.AppendLine("""<script type="text/javascript">""");
        newScript.AppendLine("""
var renderPlotly = function() {
    var xplotRequire = require.config({context:'xplot-3.0.1',paths:{plotly:'https://cdn.plot.ly/plotly-1.49.2.min'}}) || require;
    xplotRequire(['plotly'], function(Plotly) { 
""");
        newScript.AppendLine(script);
        newScript.AppendLine(@"});
};");
        newScript.AppendLine(JavascriptUtilities.GetCodeForEnsureRequireJs(
            new Uri("https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js"), "renderPlotly"));
        newScript.AppendLine("</script>");
        return newScript.ToString();
    }

    public static HtmlString GetNotebookHtml(this PlotlyChart chart)
    {
        var styleStr = $"width: {chart.Width}px; height: {chart.Height}px;";
        var div =
            HtmlElements.div.Invoke(new FSharpList<HtmlElements.XmlAttribute>(
                HtmlElements.XmlAttribute.NewKeyValue("style", styleStr),
                FSharpList<HtmlElements.XmlAttribute>.Cons(HtmlElements.XmlAttribute.NewKeyValue("id", chart.Id),
                    FSharpList<HtmlElements.XmlAttribute>.Empty)
            )).Invoke(FSharpList<HtmlElements.XmlNode>.Empty);
        var divElem = RenderView.AsString.htmlDocument(div);

        var js = chart.GetInlineJS()
            .Replace("<script>", String.Empty)
            .Replace("</script>", String.Empty);
        return new HtmlString(divElem + GetScriptElementWithRequire(js));
    }

    public static void DisplayOnNotebook(this PlotlyChart chart)
    {
        chart
            .GetNotebookHtml()
            .ToString()
            .DisplayAs(HtmlFormatter.MimeType);
    }
}