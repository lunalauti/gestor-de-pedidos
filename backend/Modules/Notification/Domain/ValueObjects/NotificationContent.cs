namespace Notification.Domain.ValueObjects
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

        public static NotificationContent CreateOrderNotification(string orderId, string eventType, string customerName, string externalId)
        {
            var (title, body) = GetContentByEventType(eventType, externalId, customerName);
            
            return new NotificationContent(title, body, new Dictionary<string, string>
            {
                { "orderId", orderId },
                { "eventType", eventType },
                { "customerName", customerName },
                { "externalId", externalId }
            });
        }

        private static (string title, string body) GetContentByEventType(string eventType, string externalId, string customerName)
        {
            return eventType switch
            {
                "ORDER_RECEIVED" => ("Nuevo Pedido", $"Pedido #{externalId} recibido - {customerName}"),
                "ORDER_READY" => ("Pedido Listo", $"Pedido #{externalId} listo para envío"),
                "ORDER_ASSIGNED" => ("Pedido Asignado", $"Te asignaron el pedido #{externalId}"),
                "ORDER_OUT_FOR_DELIVERY" => ("En Camino", $"Pedido #{externalId} en camino"),
                "ORDER_DELIVERED" => ("Entregado", $"Pedido #{externalId} entregado exitosamente"),
                "ORDER_FAILED" => ("Fallo en Entrega", $"No se pudo entregar el pedido #{externalId}"),
                _ => ("Actualización de Pedido", $"Cambios en pedido #{externalId}")
            };
        }
    }
}