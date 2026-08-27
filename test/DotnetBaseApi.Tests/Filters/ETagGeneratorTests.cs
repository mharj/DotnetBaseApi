using Xunit;
using DotnetBaseApi.Filters;

namespace DotnetBaseApi.Tests.Filters
{
    public class ETagGeneratorTests
    {
        [Fact]
        public void GenerateETag_Returns_Consistent_Hash_For_Same_Data()
        {
            // Arrange
            var data = System.Text.Encoding.UTF8.GetBytes("{\"id\":1,\"name\":\"Test Book\"}");

            // Act
            var etag1 = ETagGenerator.GenerateETag(data);
            var etag2 = ETagGenerator.GenerateETag(data);

            // Assert
            Assert.Equal(etag1, etag2);
            Assert.False(string.IsNullOrEmpty(etag1));
        }

        [Fact]
        public void GenerateETag_Returns_Different_Hash_For_Different_Data()
        {
            // Arrange
            var data1 = System.Text.Encoding.UTF8.GetBytes("{\"id\":1,\"name\":\"Test Book\"}");
            var data2 = System.Text.Encoding.UTF8.GetBytes("{\"id\":2,\"name\":\"Different Book\"}");

            // Act
            var etag1 = ETagGenerator.GenerateETag(data1);
            var etag2 = ETagGenerator.GenerateETag(data2);

            // Assert
            Assert.NotEqual(etag1, etag2);
        }

        [Fact]
        public void GenerateETag_Returns_Uppercase_Hex_String()
        {
            // Arrange
            var data = System.Text.Encoding.UTF8.GetBytes("test data");

            // Act
            var etag = ETagGenerator.GenerateETag(data);

            // Assert
            Assert.Matches("^\"[A-F0-9]+\"$", etag);
        }
                [Fact]
        public void GenerateWeakETag_Returns_Uppercase_Hex_String()
        {
            // Arrange
            var data = System.Text.Encoding.UTF8.GetBytes("test data");

            // Act
            var etag = ETagGenerator.GenerateETag(data, weak: true);

            // Assert
            Assert.Matches("^W/\"[A-F0-9]+\"$", etag);
        }
    }
}
