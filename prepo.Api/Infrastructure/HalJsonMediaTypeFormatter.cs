﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using Codeite.Core.Json;
using Newtonsoft.Json;
using prepo.Api.Resources;

namespace prepo.Api.Infrastructure
{
    public class HalJsonMediaTypeFormatter : BufferedMediaTypeFormatter
    {
        public HalJsonMediaTypeFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
            this.SupportedMediaTypes.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public override bool CanReadType(Type type)
        {
            var cwt = typeof(HalResource).IsAssignableFrom(type);
            return cwt;
        }

        public override bool CanWriteType(Type type)
        {
            var cwt = typeof(HalResource).IsAssignableFrom(type);
            return cwt;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content)
        {
            var halResource = value as HalResource;

            if (halResource == null)
            {
                return;
            }

            var writer = new StreamWriter(writeStream);

            var json = halResource.ToDynamicJson().ToJsonString();
            writer.Write(json);
            writer.Flush();
            return;

            var jsonTextWriter = new JsonTextWriter(writer);

            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("_links");
            jsonTextWriter.WriteStartObject();

            WriteLink(jsonTextWriter, halResource.SelfLink);

            foreach (var relatedResource in halResource.GetRelatedResources())
            {
                WriteLink(jsonTextWriter, relatedResource);
            }

            jsonTextWriter.WriteEndObject();
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
        }

        private void WriteLink(JsonTextWriter writer, ResourceLink link)
        {
            writer.WritePropertyName(link.Name);
            writer.WriteStartObject();
            writer.WritePropertyName("href");
            writer.WriteValue(link.Href);
            writer.WriteEndObject();
        }

        public override object ReadFromStream(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return base.ReadFromStream(type, readStream, content, formatterLogger);
        }
    }
}