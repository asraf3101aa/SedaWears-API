using Microsoft.Extensions.Options;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.ExternalServices;

public class S3Service(IAmazonS3 s3Client, IOptions<S3Config> configOptions) : IS3Service
{

    public Uri GetPreSignedUrl(string contentType, string fileName)
    {
        var key = $"public/{fileName}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = configOptions.Value.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            ContentType = contentType
        };

        return new Uri(s3Client.GetPreSignedURL(request));
    }

    public async Task DeleteFileAsync(string filename)
    {
        try
        {
            var publicRequest = new DeleteObjectRequest
            {
                BucketName = configOptions.Value.BucketName,
                Key = $"public/{filename}"
            };
            await s3Client.DeleteObjectAsync(publicRequest);

            // Fire and forget cache deletion
            var cacheRequest = new DeleteObjectRequest
            {
                BucketName = configOptions.Value.BucketName,
                Key = $"cache/{filename}"
            };
            _ = s3Client.DeleteObjectAsync(cacheRequest);
        }
        catch (Exception)
        {
            // Log error or handle as needed.
        }
    }

}