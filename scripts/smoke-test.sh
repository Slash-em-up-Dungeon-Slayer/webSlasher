#!/usr/bin/env bash
# Быстрая проверка живого backend: health-check + полный цикл auth (регистрация -> логин -> токен).
# Использование:
#   BASE_URL=http://localhost:5080 ./scripts/smoke-test.sh
#   BASE_URL=https://your-real-domain ./scripts/smoke-test.sh

set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5080}"
TEST_EMAIL="smoke_$(date +%s)@example.com"
TEST_PASSWORD="TestPassword123!"

pass() { echo "  OK: $1"; }
fail() { echo "  FAIL: $1"; exit 1; }

echo "== 1. Health check ($BASE_URL/health) =="
HEALTH_STATUS=$(curl -s -o /tmp/health.json -w "%{http_code}" "$BASE_URL/health") || true
if [ "$HEALTH_STATUS" = "200" ]; then
  pass "health вернул 200: $(cat /tmp/health.json)"
else
  fail "health вернул HTTP $HEALTH_STATUS вместо 200"
fi

echo "== 2. Регистрация ($TEST_EMAIL) =="
REGISTER_STATUS=$(curl -s -o /tmp/register.json -w "%{http_code}" \
  -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}") || true

if [ "$REGISTER_STATUS" = "200" ]; then
  pass "регистрация прошла"
else
  fail "регистрация вернула HTTP $REGISTER_STATUS: $(cat /tmp/register.json)"
fi

echo "== 3. Логин =="
LOGIN_STATUS=$(curl -s -o /tmp/login.json -w "%{http_code}" \
  -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}") || true

if [ "$LOGIN_STATUS" != "200" ]; then
  fail "логин вернул HTTP $LOGIN_STATUS: $(cat /tmp/login.json)"
fi

if grep -q '"token"' /tmp/login.json; then
  pass "логин вернул JWT-токен"
else
  fail "в ответе логина нет поля token: $(cat /tmp/login.json)"
fi

echo "== 4. Повторная регистрация тем же email (ожидаем ошибку) =="
DUPLICATE_STATUS=$(curl -s -o /tmp/duplicate.json -w "%{http_code}" \
  -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}") || true

if [ "$DUPLICATE_STATUS" = "400" ]; then
  pass "повторная регистрация корректно отклонена (400)"
else
  fail "повторная регистрация вернула HTTP $DUPLICATE_STATUS, ожидался 400"
fi

echo ""
echo "Все проверки пройдены. Backend работает и auth-flow исправен."
