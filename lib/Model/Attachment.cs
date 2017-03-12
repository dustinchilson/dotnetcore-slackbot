namespace Slackbot.Model
{
    public class Attachment
    {
        public string Fallback { get; set; }
        public string Color { get; set; }
        public string Pretext { get; set; }
        public string Author_Name { get; set; }
        public string Author_Link { get; set; }
        public string Author_Icon { get; set; }
        public string Title { get; set; }
        public string Title_Link { get; set; }
        public string Text { get; set; }
        public Field[] Fields { get; set; }
        public string Image_Url { get; set; }
        public string Thumb_Url { get; set; }
        public string Footer { get; set; }
        public string Footer_Icon { get; set; }
        public int ts { get; set; }
    }
}