# Implementation Plan - Method Tracing System

- [x] 1. Create core infrastructure and interfaces





  - Create IParameterSerializer interface with Serialize and SerializeParameters methods
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [x] 2. Implement ParameterSerializer





  - [x] 2.1 Create ParameterSerializer class implementing IParameterSerializer


    - Implement sensitive parameter detection (password, secret, token, key)
    - Implement null handling
    - Implement string serialization with truncation at 200 chars
    - Implement byte array serialization (show length only)
    - Implement primitive type serialization
    - Implement collection serialization (show count and first 5 items)
    - Implement complex object serialization (show type and up to 5 properties)
    - Limit reflection depth to 2 levels
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 7.1, 7.2, 7.3, 7.4_

  - [x] 2.2 Write property test for sensitive parameter redaction


    - **Property 1: Sensitive parameter redaction**
    - **Validates: Requirements 7.1, 7.2, 7.3, 7.4**


  - [x] 2.3 Write property test for string truncation

    - **Property 2: String truncation consistency**
    - **Validates: Requirements 2.1**


  - [x] 2.4 Write property test for byte array serialization

    - **Property 3: Byte array serialization format**

    - **Validates: Requirements 2.2**

  - [x] 2.5 Write unit test for null handling


    - **Property 4: Null handling**
    - **Validates: Requirements 2.5**



  - [x] 2.6 Write property test for primitive serialization

    - Test int, long, double, decimal, bool, DateTime, DateTimeOffset
    - **Validates: Requirements 2.6**



  - [x] 2.7 Write unit tests for collection serialization


    - Test empty collections
    - Test collections with <5 items
    - Test collections with >5 items
    - **Validates: Requirements 2.3**

  - [x] 2.8 Write unit tests for complex object serialization

    - Test objects with no properties
    - Test objects with properties
    - Test objects with long property values
    - Test property reflection errors
    - **Validates: Requirements 2.4**

- [x] 3. Implement MethodTracingActionFilter





  - [x] 3.1 Create MethodTracingActionFilter class implementing IActionFilter


    - Inject ILogger and IParameterSerializer
    - Implement OnActionExecuting
      - Check if Verbose logging is enabled (short-circuit if not)
      - Extract controller name and action name
      - Serialize action parameters
      - Start stopwatch and store in HttpContext.Items
      - Log entry with format "Entering {ControllerName}.{ActionName}"
    - Implement OnActionExecuted
      - Check if Verbose logging is enabled (short-circuit if not)
      - Extract controller name and action name
      - Retrieve stopwatch from HttpContext.Items
      - Log exit with duration
      - Log exception if present
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 6.1, 6.2, 6.3, 6.4, 6.5_

  - [x] 3.2 Write property test for controller action logging


    - **Property: Controller action logging**
    - For any controller action, when invoked, method name and parameters should be logged
    - **Validates: Requirements 1.1, 1.2**

  - [x] 3.3 Write property test for duration logging

    - **Property 6: Timing accuracy**
    - **Validates: Requirements 1.4**

  - [x] 3.4 Write property test for short-circuit optimization

    - **Property 5: Logging level short-circuit**
    - **Validates: Requirements 1.5, 5.1**

  - [x] 3.5 Write unit test for exception logging

    - **Property 8: Exception logging**
    - **Validates: Requirements 6.4**

  - [x] 3.6 Write property test for log format consistency

    - **Property 7: Log format consistency**
    - **Validates: Requirements 6.1, 6.3**

- [x] 4. Implement LoggingAiClientDecorator




  - [x] 4.1 Create LoggingAiClientDecorator class implementing IAiClient


    - Inject inner IAiClient, ILogger, and IParameterSerializer
    - Implement SendPromptWithMetadata
      - Check if Verbose logging is enabled (short-circuit if not)
      - Extract service name
      - Serialize parameters (prompt, schema)
      - Log entry with format "Entering {ServiceName}.{MethodName}"
      - Start stopwatch
      - Call inner service
      - Log exit with duration
      - Log exception if thrown (then re-throw)
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 6.1, 6.3, 6.4_

  - [x] 4.2 Write property test for service method logging


    - **Property: Service method logging**
    - For any decorated service method, when invoked, method name and parameters should be logged
    - **Validates: Requirements 3.2**

  - [x] 4.3 Write property test for service duration logging

    - **Property: Service duration logging**
    - For any decorated service method, execution duration should be logged
    - **Validates: Requirements 3.3**

  - [x] 4.4 Write property test for service exception logging

    - **Property: Service exception logging**
    - For any decorated service method that throws, exception should be logged before re-throw
    - **Validates: Requirements 3.4**

  - [x] 4.5 Write unit test for short-circuit optimization

    - Test that when Verbose is disabled, no serialization occurs
    - **Validates: Requirements 5.1, 5.2**

- [x] 5. Register services in Program.cs
  - Register IParameterSerializer as Singleton
  - Register MethodTracingActionFilter globally in AddControllers
  - Update IAiClient registrations to use LoggingAiClientDecorator
    - Wrap Gemini service
    - Wrap Claude service
    - Wrap Nebius service
  - Ensure decorator chain: TimedAiClient → LoggingAiClientDecorator → Actual Service
  - _Requirements: 3.1, 5.2_

- [x] 6. Update appsettings for Verbose logging
  - Add Verbose level override for PocketLawyer namespace in appsettings.Development.json
  - Document how to enable/disable method tracing
  - Add examples of selective tracing by namespace
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [x] 7. Write integration tests
  - Test end-to-end controller action tracing
  - Test end-to-end service method tracing
  - Test that logs appear in output at Verbose level
  - Test that no logs appear when Verbose is disabled
  - _Requirements: 1.1, 1.2, 1.4, 3.2, 3.3_

- [x] 8. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 9. Performance testing
  - Measure overhead when Verbose logging is disabled (<0.1%)
  - Measure overhead when Verbose logging is enabled (<5%)
  - Measure serialization time for various parameter types
  - Verify short-circuit optimization works
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 10. Update documentation
  - Update CONFIGURATION.md with Verbose logging configuration
  - Add section explaining method tracing
  - Add examples of enabling tracing for specific namespaces
  - Document performance impact
  - Document sensitive parameter redaction
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 11. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
