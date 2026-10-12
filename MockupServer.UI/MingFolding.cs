using ICSharpCode.TextEditor.Document;
using System.Text.RegularExpressions;

namespace MockupServer.UI
{
    public class MingFolding : IFoldingStrategy
    {
        const int FoldEndColumn = 57;

        /// <summary>
        /// Generates the foldings for our document.
        /// </summary>
        /// <param name="document">The current document.</param>
        /// <param name="fileName">The filename of the document.</param>
        /// <param name="parseInformation">Extra parse information, not used in this sample.</param>
        /// <returns>A list of FoldMarkers.</returns>
        public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
        {
            List<FoldMarker> list = new List<FoldMarker>();
            var startLines = new Stack<int>();
            // Create foldmarkers for the whole document, enumerate through every line.
            for (int i = 0; i < document.TotalNumberOfLines; i++)
            {
                string text = document.GetText(document.GetLineSegment(i)).Trim();

                if (text.StartsWith("#region"))
                {
                    startLines.Push(i);
                }
                if (text.StartsWith("#endregion") && startLines.Count > 0)
                {
                    int start = startLines.Pop();
                    list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, FoldEndColumn, FoldType.Region, "..."));
                }
                //支持嵌套 {}
                if (text.StartsWith("{"))
                {
                    startLines.Push(i);
                }
                if (text.StartsWith("}") && startLines.Count > 0)
                {
                    int start = startLines.Pop();
                    list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, FoldEndColumn, FoldType.TypeBody, "...}"));
                }

                if (Regex.IsMatch(text, @"\<[a-zA-Z0-9\-_\$ ]*\>"))
                {
                    startLines.Push(i);
                }
                if (Regex.IsMatch(text, @"\</[a-zA-Z0-9\-_\$ ]*\>") && startLines.Count > 0)
                {
                    int start = startLines.Pop();
                    if (start != i)
                        list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, FoldEndColumn, FoldType.TypeBody, "..."));
                }

                if (text.StartsWith("/// <summary>"))
                {
                    startLines.Push(i);
                }
                if (text.StartsWith("/// <returns>") && startLines.Count > 0)
                {
                    int start = startLines.Pop();
                    //获取注释文本（包括空格）
                    string display = document.GetText(document.GetLineSegment(start + 1).Offset, document.GetLineSegment(start + 1).Length);
                    //remove ///
                    display = display.Trim().TrimStart('/');
                    list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, FoldEndColumn, FoldType.TypeBody, display));
                }
            }

            return list;
        }
    }
}
