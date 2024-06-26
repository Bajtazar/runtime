// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.Http.Functional.Tests
{
    public class StringContentTest
    {
        [Fact]
        public void Ctor_NullString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new StringContent(null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Ctor_EmptyString_Accept(bool readStreamAsync)
        {
            // Consider empty strings like null strings (null and empty strings should be treated equally).
            var content = new StringContent(string.Empty);
            Stream result = await content.ReadAsStreamAsync(readStreamAsync);
            Assert.Equal(0, result.Length);
        }

        [Fact]
        public async Task Ctor_UseCustomEncodingAndMediaType_EncodingUsedAndContentTypeHeaderUpdated()
        {
            // Use UTF-8 encoding to serialize a chinese string.
            string sourceString = "\u4f1a\u5458\u670d\u52a1";

            var content = new StringContent(sourceString, Encoding.UTF8, "application/custom");

            Assert.Equal("application/custom", content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);

            var destination = new MemoryStream(12);
            await content.CopyToAsync(destination);

            string destinationString = Encoding.UTF8.GetString(destination.ToArray(), 0, (int)destination.Length);

            Assert.Equal(sourceString, destinationString);
        }

        [Fact]
        public async Task Ctor_DefineNoEncoding_DefaultEncodingUsed()
        {
            string sourceString = "\u00C4\u00E4\u00FC\u00DC";
            var content = new StringContent(sourceString);
            Encoding defaultStringEncoding = Encoding.GetEncoding("utf-8");

            // If no encoding is defined, the default encoding is used: utf-8
            Assert.Equal("text/plain", content.Headers.ContentType.MediaType);
            Assert.Equal(defaultStringEncoding.WebName, content.Headers.ContentType.CharSet);

            // Make sure the default encoding is also used when serializing the content.
            var destination = new MemoryStream();
            await content.CopyToAsync(destination);

            Assert.Equal(8, destination.Length);

            destination.Seek(0, SeekOrigin.Begin);
            string roundTrip = new StreamReader(destination, defaultStringEncoding).ReadToEnd();
            Assert.Equal(sourceString, roundTrip);
        }

        [Fact]
        public void Ctor_SetsDefaultContentTypeHeader()
        {
            var content = new StringContent("Foo");
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", (Encoding)null);
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", Encoding.UTF8);
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", Encoding.UTF8, "text/plain");
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", null, "text/plain");
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", Encoding.UTF8, (string)null);
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", null, (string)null);
            Assert.Equal("text/plain; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("Foo", (MediaTypeHeaderValue)null);
            Assert.Null(content.Headers.ContentType);

            content = new StringContent("Foo", null, (MediaTypeHeaderValue)null);
            Assert.Null(content.Headers.ContentType);
        }

        [Theory]
        [InlineData("text/plain")]
        [InlineData("application/json")]
        [InlineData("application/xml")]
        [InlineData("foo/bar")]
        public void Ctor_SetsContentTypeHeader(string mediaType)
        {
            var content = new StringContent("foo", Encoding.UTF8, mediaType);
            Assert.Equal($"{mediaType}; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("foo", null, mediaType);
            Assert.Equal($"{mediaType}; charset=utf-8", content.Headers.ContentType.ToString());

            content = new StringContent("foo", Encoding.ASCII, mediaType);
            Assert.Equal($"{mediaType}; charset=us-ascii", content.Headers.ContentType.ToString());

            content = new StringContent("foo", new MediaTypeHeaderValue(mediaType));
            Assert.Equal(mediaType, content.Headers.ContentType.ToString());

            content = new StringContent("foo", Encoding.UTF8, new MediaTypeHeaderValue(mediaType, "ascii"));
            Assert.Equal($"{mediaType}; charset=ascii", content.Headers.ContentType.ToString());
        }

        [Fact]
        public void Ctor_PassNullHeaderValueForMediaType_NoMediaTypeUsed()
        {
            string sourceString = "\u00C4\u00E4\u00FC\u00DC";
            Encoding defaultStringEncoding = Encoding.GetEncoding("utf-8");
            var content = new StringContent(sourceString, defaultStringEncoding, (MediaTypeHeaderValue)null);

            // If no media header value is passed-in, there is none
            Assert.Null(content.Headers.ContentType);
        }

        [Fact]
        public async Task Ctor_UseCustomMediaTypeHeaderValue_SpecificEncoding()
        {
            // Use UTF-8 encoding to serialize a chinese string.
            string sourceString = "\u4f1a\u5458\u670d\u52a1";

            var mediaTypeHeaderValue = new Headers.MediaTypeHeaderValue("application/custom", Encoding.UTF8.WebName);

            var content = new StringContent(sourceString, Encoding.UTF8, mediaTypeHeaderValue);

            Assert.Equal("application/custom", content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);

            var destination = new MemoryStream(12);
            await content.CopyToAsync(destination);

            string destinationString = Encoding.UTF8.GetString(destination.ToArray(), 0, (int)destination.Length);

            Assert.Equal(sourceString, destinationString);
        }

        [Fact]
        public async Task Ctor_UseCustomMediaTypeHeaderValue()
        {
            // Use UTF-8 encoding to serialize a chinese string.
            string sourceString = "\u4f1a\u5458\u670d\u52a1";

            var mediaTypeHeaderValue = new Headers.MediaTypeHeaderValue("application/custom", Encoding.UTF8.WebName);

            var content = new StringContent(sourceString, mediaTypeHeaderValue);

            Assert.Equal("application/custom", content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);

            var destination = new MemoryStream(12);
            await content.CopyToAsync(destination);

            string destinationString = Encoding.UTF8.GetString(destination.ToArray(), 0, (int)destination.Length);

            Assert.Equal(sourceString, destinationString);
        }

        [Fact]
        public async Task Ctor_UseSpecificEncodingAndContentType()
        {
            // Use UTF-8 encoding to serialize a chinese string.
            string sourceString = "\u4f1a\u5458\u670d\u52a1";
            string contentType = "application/custom";

            var content = new StringContent(sourceString, Encoding.UTF8, contentType);

            Assert.Equal("application/custom", content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);

            var destination = new MemoryStream(12);
            await content.CopyToAsync(destination);

            string destinationString = Encoding.UTF8.GetString(destination.ToArray(), 0, (int)destination.Length);

            Assert.Equal(sourceString, destinationString);
        }
    }
}
