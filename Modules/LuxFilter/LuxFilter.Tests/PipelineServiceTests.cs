/*
namespace LuxFilter.Tests
{
    /// <summary>
    /// Unit tests for the PipelineService class.
    /// </summary>
    public class PipelineServiceTests
    {
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly PipelineService _pipelineService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineServiceTests"/> class.
        /// </summary>
        public PipelineServiceTests()
        {
            _mockLogger = new Mock<ILoggerService>();
            _pipelineService = new PipelineService(_mockLogger.Object);
        }

        /// <summary>
        /// Tests whether an algorithm can be successfully added to the pipeline.
        /// </summary>
        [Fact]
        public void AddAlgorithm_ShouldAddAlgorithmSuccessfully()
        {
            var mockAlgorithm = new Mock<IFilterAlgorithm>();
            mockAlgorithm.Setup(a => a.Name).Returns("MockAlgorithm");

            _pipelineService.AddAlgorithm(mockAlgorithm.Object, 0.5);
        }

        /// <summary>
        /// Tests whether the Compute method correctly calculates scores for images.
        /// </summary>
        [Fact]
        public async Task Compute_ShouldReturnCorrectScores()
        {
            var resolutionAlgo = new ResolutionAlgo();
            var bitmap = new SKBitmap(100, 100);
            var guid = Guid.NewGuid();

            _pipelineService.AddAlgorithm(resolutionAlgo, 1.0);
            var results = await _pipelineService.Compute([(guid, new ImageData(bitmap, FileExtension.UNKNOWN))]);

            Assert.Single(results);
            Assert.Equal(guid, results.First().Item1);
            Assert.Equal(10000, results.First().Item2["Resolution"]);
        }
    }
}
*/