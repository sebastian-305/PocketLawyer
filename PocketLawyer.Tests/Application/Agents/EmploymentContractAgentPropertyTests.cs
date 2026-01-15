using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using Xunit;

namespace PocketLawyer.Tests.Application.Agents;

/// <summary>
/// Property-based tests for EmploymentContractAgent construction.
/// Feature: employment-contract-agent
/// </summary>
public class EmploymentContractAgentPropertyTests
{
    /// <summary>
    /// **Feature: employment-contract-agent, Property 1: Unique Agent IDs**
    /// For any sequence of agent constructions, each agent should have a unique Agent_Id 
    /// that differs from all previously created agents.
    /// **Validates: Requirements 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UniqueAgentIds_EachAgentHasUniqueId()
    {
        // Generate a number of agents to create (between 2 and 20)
        var countGen = Gen.Choose(2, 20);

        return Prop.ForAll(
            countGen.ToArbitrary(),
            (count) =>
            {
                // Arrange - Create multiple agents
                var agentIds = new HashSet<string>();
                var agents = new List<EmploymentContractAgent>();

                // Act - Create agents and collect their IDs
                for (int i = 0; i < count; i++)
                {
                    var agent = CreateAgent();
                    agents.Add(agent);
                    agentIds.Add(agent.AgentId);
                }

                // Assert - All agent IDs should be unique
                // If all IDs are unique, the set size should equal the number of agents
                var allIdsUnique = agentIds.Count == count;

                // Cleanup
                agents.Clear();

                return allIdsUnique;
            });
    }

    /// <summary>
    /// **Feature: employment-contract-agent, Property 2: Null Dependency Rejection**
    /// For any combination of constructor dependencies where at least one is null, 
    /// the constructor should throw an ArgumentNullException.
    /// **Validates: Requirements 1.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NullDependencyRejection_ThrowsArgumentNullException()
    {
        // Generate which dependency to make null (0-6 for the 7 dependencies)
        var nullDependencyIndexGen = Gen.Choose(0, 6);

        return Prop.ForAll(
            nullDependencyIndexGen.ToArbitrary(),
            (nullDependencyIndex) =>
            {
                // Arrange - Create mocks for all dependencies
                var mockAiClient = new Mock<IAiClient>();
                var mockSchemaProvider = new Mock<ISchemaProvider>();
                var mockSchemaValidator = new Mock<ISchemaValidator>();
                var mockContextProvider = new Mock<IContextProvider>();
                var mockKvDetectionService = new Mock<IKvDetectionService>();
                var mockKvService = new Mock<IKollektivvertragService>();
                var mockLogger = new Mock<ILogger<EmploymentContractAgent>>();

                // Set one dependency to null based on the generated index
                IAiClient? aiClient = nullDependencyIndex == 0 ? null : mockAiClient.Object;
                ISchemaProvider? schemaProvider = nullDependencyIndex == 1 ? null : mockSchemaProvider.Object;
                ISchemaValidator? schemaValidator = nullDependencyIndex == 2 ? null : mockSchemaValidator.Object;
                IContextProvider? contextProvider = nullDependencyIndex == 3 ? null : mockContextProvider.Object;
                IKvDetectionService? kvDetectionService = nullDependencyIndex == 4 ? null : mockKvDetectionService.Object;
                IKollektivvertragService? kvService = nullDependencyIndex == 5 ? null : mockKvService.Object;
                ILogger<EmploymentContractAgent>? logger = nullDependencyIndex == 6 ? null : mockLogger.Object;

                // Act & Assert - Constructor should throw ArgumentNullException
                try
                {
                    var agent = new EmploymentContractAgent(
                        aiClient!,
                        schemaProvider!,
                        schemaValidator!,
                        contextProvider!,
                        kvDetectionService!,
                        kvService!,
                        logger!);

                    // If we reach here, no exception was thrown - test fails
                    return false;
                }
                catch (ArgumentNullException)
                {
                    // Expected exception - test passes
                    return true;
                }
                catch (Exception)
                {
                    // Wrong exception type - test fails
                    return false;
                }
            });
    }

    /// <summary>
    /// Helper method to create an EmploymentContractAgent with valid mocked dependencies.
    /// </summary>
    private static EmploymentContractAgent CreateAgent()
    {
        var mockAiClient = new Mock<IAiClient>();
        var mockSchemaProvider = new Mock<ISchemaProvider>();
        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = new Mock<IContextProvider>();
        var mockKvDetectionService = new Mock<IKvDetectionService>();
        var mockKvService = new Mock<IKollektivvertragService>();
        var mockLogger = new Mock<ILogger<EmploymentContractAgent>>();

        return new EmploymentContractAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            mockKvDetectionService.Object,
            mockKvService.Object,
            mockLogger.Object);
    }
}
