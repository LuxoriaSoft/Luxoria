using System.Diagnostics;
using Minio;
using Minio.DataModel.Args;

namespace LuxAPI.Services
{
    public class MinioService
    {
        private readonly IMinioClient _client;

        public MinioService(IConfiguration config)
        {
            _client = new MinioClient()
                .WithEndpoint(config["Minio:Endpoint"])
                .WithCredentials(config["Minio:AccessKey"], config["Minio:SecretKey"])
                .Build();

            // Buckets initialisation process
            // Create these buckets if they do not exist
            List<string> requiredBuckets = ["user-files", "photos-bucket"];
            
            foreach (var bucket in requiredBuckets)
            {
                Console.WriteLine($"Checking bucket [{bucket}] : status=");
                if (!_client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket)).Result)
                {
                    // Create the bucket
                    _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket)).Wait();
                    Console.WriteLine("Created!");
                }
                Console.WriteLine("Already exists!");
            }
        }

        public async Task UploadFileAsync(string bucketName, string objectPath, Stream data, string contentType)
        {
            var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!exists)
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));

            var putArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectPath)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType);

            await _client.PutObjectAsync(putArgs);
        }

        public async Task<Stream?> GetFileAsync(string bucketName, string objectPath)
        {
            var ms = new MemoryStream();

            try
            {
                await _client.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectPath)
                    .WithCallbackStream(stream => stream.CopyTo(ms)));
                ms.Position = 0;
                return ms;
            }
            catch
            {
                return null;
            }
        }

        public async Task DeleteFileAsync(string bucketName, string objectPath)
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectPath));
        }
    }
}
