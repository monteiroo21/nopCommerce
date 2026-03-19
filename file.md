Event Publisher -> used for code decoupling
If it wasn't implemented, for example order processing service class would need to know about other Service Dependencies

IServiceProvider -> provides on-demand access to other classes
There's two ways to get another class in the code:
-> Constructor Injection, as seen in OrderProcessingService.cs, where the system knows which classes he needs to inject
-> Asking IServiceProvider to find every single consumer for a specific event dynamically at runtime, since it can't inject evert single consumer needed into its constructor, as seen in the IEventPublisher.cs implementation