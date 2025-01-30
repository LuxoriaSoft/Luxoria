using LuxFilter.Algorithms.ImageQuality;
using LuxFilter.Algorithms.Interfaces;
using LuxFilter.Services;
using Luxoria.SDK.Interfaces;
using Moq;
using SkiaSharp;

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
            var results = await _pipelineService.Compute(new List<(Guid, SKBitmap)> { (guid, bitmap) });

            Assert.Single(results);
            Assert.Equal(guid, results[0].Item1);
            Assert.Equal(10000, results[0].Item2);
        }
    }

    /// <summary>
    /// Unit tests for the ResolutionAlgo class.
    /// </summary>
    public class ResolutionAlgoTests
    {
        /// <summary>
        /// Tests whether the Compute method returns the correct resolution value.
        /// </summary>
        [Fact]
        public void Compute_ShouldReturnCorrectResolution()
        {
            var algorithm = new ResolutionAlgo();
            var bitmap = new SKBitmap(50, 40);

            var result = algorithm.Compute(bitmap, 50, 40);
            Assert.Equal(2000, result);
        }

        /// <summary>
        /// Tests whether the Compute method handles cases where width or height is zero.
        /// </summary>
        [Fact]
        public void Compute_ShouldHandleZeroWidthOrHeight()
        {
            var algorithm = new ResolutionAlgo();
            var zeroWidthBitmap = new SKBitmap(0, 40);
            var zeroHeightBitmap = new SKBitmap(50, 0);

            var resultZeroWidth = algorithm.Compute(zeroWidthBitmap, 0, 40);
            var resultZeroHeight = algorithm.Compute(zeroHeightBitmap, 50, 0);

            Assert.Equal(0, resultZeroWidth);
            Assert.Equal(0, resultZeroHeight);
        }
    }

    /// <summary>
    /// Unit tests for the SharpnessAlgo class.
    /// </summary>
    public class SharpnessAlgoTests
    {
        /// <summary>
        /// Tests whether the Compute method returns a non-negative sharpness score.
        /// </summary>
        [Fact]
        public void Compute_ShouldReturnSharpnessScore()
        {
            var algorithm = new SharpnessAlgo();
            var bitmap = new SKBitmap(50, 40);

            var result = algorithm.Compute(bitmap, 50, 40);
            Assert.True(result >= 0);
        }
    }
}