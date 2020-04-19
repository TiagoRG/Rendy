using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Rendy
{
    public class Embed : IEmbed
    {
        public EmbedAuthor? Author { get; }
        public Color? Color { get; }
        public string Description { get; }
        public ImmutableArray<EmbedField> Fields { get; }
        public EmbedFooter? Footer { get; }
        public EmbedImage? Image { get; }
        public EmbedProvider? Provider { get; }
        public EmbedThumbnail? Thumbnail { get; }
        public DateTimeOffset? Timestamp { get; }
        public string Title { get; }
        public EmbedType Type { get; }
        public string Url { get; }
        public EmbedVideo? Video { get; }
        internal Embed(EmbedType type)
        {
            Type = type;
            Fields = ImmutableArray.Create<EmbedField>();
        }
        internal Embed(EmbedType type,
            string title,
            string description,
            string url,
            DateTimeOffset? timestamp,
            Color? color,
            EmbedImage? image,
            EmbedVideo? video,
            EmbedAuthor? author,
            EmbedFooter? footer,
            EmbedProvider? provider,
            EmbedThumbnail? thumbnail,
            ImmutableArray<EmbedField> fields)
        {
            Type = type;
            Title = title;
            Description = description;
            Url = url;
            Color = color;
            Timestamp = timestamp;
            Image = image;
            Video = video;
            Author = author;
            Footer = footer;
            Provider = provider;
            Thumbnail = thumbnail;
            Fields = fields;
        }

        public int Length
        {
            get
            {
                int titleLength = Title?.Length ?? 0;
                int authorLength = Author?.Name?.Length ?? 0;
                int descriptionLength = Description?.Length ?? 0;
                int footerLength = Footer?.Text?.Length ?? 0;
                int fieldSum = Fields.Sum(f => f.Name?.Length + f.Value?.ToString().Length) ?? 0;
                return titleLength + authorLength + descriptionLength + footerLength + fieldSum;
            }
        }

        /// <summary>
        ///     Gets the title of the embed.
        /// </summary>
        public override string ToString() => Title;
        private string DebuggerDisplay => $"{Title} ({Type})";
    }
}
