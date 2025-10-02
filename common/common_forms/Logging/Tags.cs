namespace common_forms.Logging
{
    public enum TextTag { None, Bold, Underline, Strikeout, Italic, Red, Green, Blue, Orange, Violet, Yellow, Cyan, Lime }

    internal class Tags
    {
        internal string WrapInTag(string text, TextTag tag)
        {
            return $"{StartingTag(tag)}{text}{EndingTag(tag)}";
        }

        internal string StartingTag(TextTag tag)
        {
            return $"<{tag}>";
        }

        internal string EndingTag(TextTag tag)
        {
            return $"</{tag}>";
        }
    }
}
