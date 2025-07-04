# Testing Policy for **Luxoria**

## **Objective**
The purpose of this testing policy is to establish a clear and robust framework for writing, organizing, and maintaining unit tests for the **Luxoria** project. This ensures consistent quality, high coverage, and minimal regressions throughout development.

Please refer to [Luxoria UI Manual Test Protocol](./MTP1.md), to test / assert our Luxoria Desktop Application

---

## **Core Principles**

1. **High Coverage:**
    - Ensure all critical components and workflows are covered, including normal, edge, and error scenarios.

2. **Test Isolation:**
    - Each unit test must run independently of others. External dependencies must be mocked or stubbed.

3. **Performance:**
    - Tests should execute quickly to support continuous testing during development.

4. **Readability:**
    - Test code should be self-explanatory, with clear naming conventions and comments where needed.

5. **Ease of Maintenance:**
    - Tests must be straightforward to update as the code evolves. Avoid excessive coupling between tests and implementation details.

---

## **Test Structure**

### **1. Organization**
- Tests should mirror the structure of the source code:
    - `Luxoria.Modules` → `Luxoria.Modules.Tests`
- Each class or module should have a dedicated test file named `ClassNameTests.cs`.

### **2. Naming Conventions**
- Tests should clearly describe their intent using the format:
    - `MethodName_StateUnderTest_ExpectedBehavior`
    - Example: `Subscribe_WithValidHandler_ShouldAddSubscriber`

### **3. Test Scenarios**
Tests must cover:
- **Standard Cases:** Typical input and expected behavior.
- **Edge Cases:** Extreme conditions such as null values, empty lists, or large inputs.
- **Error Cases:** Behavior when exceptions are expected or invalid data is provided.

---

## **Testing Best Practices**

### **1. Use of Mocking**
- Use mocking libraries (e.g., Moq) to isolate dependencies.
- Verify mock interactions to ensure proper communication between components.

### **2. Centralized Setup**
- Common dependencies should be initialized in a shared setup method (e.g., constructors or `[SetUp]` in test frameworks).

### **3. Assertions**
- Assertions must be explicit and descriptive to validate expected outcomes:
    - Use `Assert.Equal`, `Assert.NotNull`, and `Mock.Verify` for clarity.

### **4. Test-Driven Development (TDD)**
- Write tests before implementing the functionality to capture business requirements effectively.

### **5. Handle Exceptional Scenarios**
- Ensure error handling paths are tested. For example:
    - Validate proper exceptions are thrown for invalid inputs.
    - Test fallback mechanisms where applicable.

---

## **Guidelines for Writing Unit Tests**

### **1. Test File Template**
A unit test file should follow this basic structure:

```csharp
using Xunit;
using Moq;
using Luxoria.Modules;

namespace Luxoria.Modules.Tests
{
    public class ClassNameTests
    {
        private readonly ClassName _classUnderTest;

        public ClassNameTests()
        {
            _classUnderTest = new ClassName();
        }

        [Fact]
        public void MethodName_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mockDependency = new Mock<IDependency>();
            // Set up dependencies

            // Act
            var result = _classUnderTest.MethodUnderTest();

            // Assert
            Assert.NotNull(result);
        }
    }
}
```

### **2. Examples of Improved Test Cases**

#### Case 1: Validating Null Handling
```csharp
[Fact]
public void Subscribe_NullSubscriber_ShouldThrowArgumentNullException()
{
    // Arrange
    Action<LogEvent> nullSubscriber = null;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => _eventBus.Subscribe(nullSubscriber));
}
```

#### Case 2: Testing State Changes
```csharp
[Fact]
public void Unsubscribe_NonexistentSubscriber_ShouldNotThrow()
{
    // Arrange
    var mockSubscriber = new Mock<Action<LogEvent>>();

    // Act & Assert
    _eventBus.Unsubscribe(mockSubscriber.Object); // Should not throw
}
```

---

## **Continuous Integration and Testing Automation**

- Use CI tools (e.g., GitHub Actions, Jenkins) to run unit tests automatically on every commit.
- Track test coverage metrics using tools like `coverlet` or `dotCover`.
- Set thresholds for coverage to prevent regressions (e.g., 80% minimum).

---

## **Review Process**

1. **Code Reviews:**
    - All new test code must be reviewed by peers to ensure adherence to this policy.

2. **Test Audits:**
    - Conduct periodic reviews to identify outdated or redundant tests and improve overall coverage.

3. **Refactoring:**
    - Update tests whenever code is refactored to align with new implementations.

---

## **Monitoring and Metrics**

Track the following metrics to ensure test quality:
- **Test Coverage:** Percentage of code covered by tests.
- **Test Reliability:** Percentage of flaky or unreliable tests.
- **Execution Time:** Average time taken for tests to run.

---

This policy ensures consistency and scalability for the testing practices in **Luxoria**. With proper adherence, it will facilitate long-term project stability and maintainability.
