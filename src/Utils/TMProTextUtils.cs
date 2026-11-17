namespace BluePrinceArchipelago.Utils
{
	public static class TMProTextUtils
	{
		private static string ApplyRichTextTag(string text, string tag, string attribute = "")
		{
			if (attribute.Length == 0)
			{
				return $"<{tag}>{text}</{tag}>";
			}
			return $"<{tag}={attribute}>{text}</{tag}>";
		}

		public static string ColorText(string text, string color)
		{
			return ApplyRichTextTag(text, "color", color);
		}

		public static string StrikethroughText(string text)
		{
			return ApplyRichTextTag(text, "s");
		}
	}
}
