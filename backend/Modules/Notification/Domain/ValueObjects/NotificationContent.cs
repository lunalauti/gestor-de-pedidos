namespace Notifications.Domain.ValueObjects
{
    public class NotificationContent
    {
        public string Title { get; private set; }
        public string Body { get; private set; }
        public Dictionary<string, string> Data { get; private set; }

        public NotificationContent(string title, string body, Dictionary<string, string>? data = null)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Data = data ?? new Dictionary<string, string>();
        }

        public static NotificationContent CreateOrderNotification(Guid orderId, string eventType, string externalId)
        {
            var (title, body) = GetContentByEventType(eventType, externalId);
            
            return new NotificationContent(title, body, new Dictionary<string, string>
            {
                { "orderId", orderId.ToString() },
                { "eventType", eventType },
                { "externalId", externalId }
            });
        }

        private static (string title, string body) GetContentByEventType(string eventType, string externalId)
        {
            return eventType switch
            {
                "ORDER_RECEIVED" => ("Nuevo Pedido", $"Pedido #{externalId} recibido"),
                "ORDER_READY" => ("Pedido Listo", $"Pedido #{externalId} listo para envío"),
                "ORDER_ASSIGNED" => ("En Camino", $"Pedido #{externalId} en camino"),
                "ORDER_DELIVERED" => ("Entregado", $"Pedido #{externalId} entregado exitosamente"),
                "ORDER_FAILED" => ("Fallo en Entrega", $"No se pudo entregar el pedido #{externalId}"),
                _ => ("Actualización de Pedido", $"Cambios en pedido #{externalId}")
            };
        }
    }
}