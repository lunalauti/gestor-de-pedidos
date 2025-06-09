#!/bin/bash

# Configuración
BASE_URL="https://localhost:7001"  # Cambia por tu URL
TOKEN_FCM="eMfilBNTRLieOl4wkyRiVH:APA91bGTZOr8Yh4K31nUnuv8o_b8m-FwAfNOpyQjTt6QHjFKxJkUVhd2WUjefFft8fdBfhm7dyJ4uR8sPPHYNNOdatyMZpYi_C_P2aC2txq-Fmxfu-lBKms"

echo "🚀 Iniciando pruebas de notificaciones push..."
echo "================================================"

# 1. Registrar el token de prueba
echo "📝 1. Registrando token de prueba..."
curl -X POST "$BASE_URL/api/TestNotification/register-test-token" \
  -H "Content-Type: application/json" \
  -k -s | jq '.'

echo -e "\n⏳ Esperando 2 segundos...\n"
sleep 2

# 2. Enviar notificación básica de prueba
echo "📱 2. Enviando notificación básica de prueba..."
curl -X POST "$BASE_URL/api/TestNotification/send-test-notification" \
  -H "Content-Type: application/json" \
  -k -s | jq '.'

echo -e "\n⏳ Esperando 3 segundos...\n"
sleep 3

# 3. Enviar notificación de nuevo pedido
echo "🆕 3. Enviando notificación de nuevo pedido..."
curl -X POST "$BASE_URL/api/TestNotification/send-order-notification-test" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-TEST-001",
    "eventType": "ORDER_RECEIVED",
    "targetRole": 1
  }' \
  -k -s | jq '.'

echo -e "\n⏳ Esperando 3 segundos...\n"
sleep 3

# 4. Enviar notificación de pedido listo
echo "✅ 4. Enviando notificación de pedido listo..."
curl -X POST "$BASE_URL/api/TestNotification/send-order-notification-test" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-TEST-001",
    "eventType": "ORDER_READY",
    "targetRole": 1
  }' \
  -k -s | jq '.'

echo -e "\n⏳ Esperando 3 segundos...\n"
sleep 3

# 5. Enviar notificación personalizada
echo "🎨 5. Enviando notificación personalizada..."
curl -X POST "$BASE_URL/api/TestNotification/send-custom-notification" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "🎉 ¡Notificación Especial!",
    "body": "Esta es una notificación completamente personalizada con emojis 🚀",
    "userId": "TEST_USER_001",
    "data": {
      "category": "special",
      "priority": "high",
      "action_url": "https://example.com/special"
    }
  }' \
  -k -s | jq '.'

echo -e "\n✅ ¡Pruebas completadas!"
echo "📱 Revisa tu dispositivo móvil para ver las notificaciones"
echo "================================================"