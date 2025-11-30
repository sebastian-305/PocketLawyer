namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Provides intelligent serialization of method parameters for logging purposes.
/// Handles strings, primitives, collections, byte arrays, and complex objects.
/// Automatically redacts sensitive parameters (password, token, key, secret).
/// </summary>
public interface IParameterSerializer
{
    /// <summary>
    /// Serializes a parameter value to a loggable string representation.
    /// Handles strings, primitives, collections, byte arrays, and complex objects.
    /// Redacts sensitive parameters (password, token, key, secret).
    /// </summary>
    /// <param name="parameterName">The name of the parameter (used for sensitive detection)</param>
    /// <param name="value">The parameter value to serialize</param>
    /// <returns>A string representation suitable for logging</returns>
    string Serialize(string parameterName, object? value);
    
    /// <summary>
    /// Serializes multiple parameters to a dictionary for structured logging.
    /// </summary>
    /// <param name="parameters">Dictionary of parameter names and values</param>
    /// <returns>Dictionary of parameter names and their serialized string representations</returns>
    Dictionary<string, string> SerializeParameters(IDictionary<string, object?> parameters);
}
