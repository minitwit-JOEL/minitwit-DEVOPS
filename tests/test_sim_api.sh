#!/bin/bash

# Base URL and common headers
if [ $1 == "--production" ]; then
  BASE_URL="http://68.183.67.68:8080/api/sim"
else 
  BASE_URL="https://localhost:8080/api/sim"
fi
AUTH="Authorization: Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh"
CONTENT_TYPE="Content-Type: application/json"
CONNECTION="Connection: close"

echo "=== Test 1: Update LATEST via POST /register?latest=1337 and GET /latest ==="

echo "POST /register?latest=1337"
curl -k -X POST "$BASE_URL/register?latest=1337" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"username": "test", "email": "test@test", "pwd": "foo"}'
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 2: Register User 'a' with latest=1 ==="

echo "POST /register?latest=1"
curl -k -X POST "$BASE_URL/register?latest=1" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"username": "a", "email": "a@a.a", "pwd": "a"}'
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 3: Create Message for User 'a' with latest=2 ==="

echo "POST /msgs/a?latest=2"
curl -k -X POST "$BASE_URL/msgs/a?latest=2" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"content": "Blub!"}'
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 4: Get Latest User Messages for 'a' with latest=3 ==="

echo "GET /msgs/a?no=20&latest=3"
curl -k -X GET "$BASE_URL/msgs/a?no=20&latest=3" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 5: Get Latest Messages for all users with latest=4 ==="

echo "GET /msgs?no=20&latest=4"
curl -k -X GET "$BASE_URL/msgs?no=20&latest=4" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 6: Register User 'b' with latest=5 ==="

echo "POST /register?latest=5"
curl -k -X POST "$BASE_URL/register?latest=5" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"username": "b", "email": "b@b.b", "pwd": "b"}'
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 7: Register User 'c' with latest=6 ==="

echo "POST /register?latest=6"
curl -k -X POST "$BASE_URL/register?latest=6" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"username": "c", "email": "c@c.c", "pwd": "c"}'
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 8: User 'a' Follows Users 'b' and 'c' ==="

echo "POST /fllws/a?latest=7 (Follow b)"
curl -k -X POST "$BASE_URL/fllws/a?latest=7" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"follow": "b"}'
echo -e "\n"

echo "POST /fllws/a?latest=8 (Follow c)"
curl -k -X POST "$BASE_URL/fllws/a?latest=8" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"follow": "c"}'
echo -e "\n"

echo "GET /fllws/a?no=20&latest=9 (Get follows list)"
curl -k -X GET "$BASE_URL/fllws/a?no=20&latest=9" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n\n"


echo "=== Test 9: User 'a' Unfollows User 'b' ==="

echo "POST /fllws/a?latest=10 (Unfollow b)"
curl -k -X POST "$BASE_URL/fllws/a?latest=10" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH" \
  -d '{"unfollow": "b"}'
echo -e "\n"

echo "GET /fllws/a?no=20&latest=11 (Get updated follows list)"
curl -k -X GET "$BASE_URL/fllws/a?no=20&latest=11" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n"

echo "GET /latest"
curl -k -X GET "$BASE_URL/latest" \
  -H "$CONNECTION" \
  -H "$CONTENT_TYPE" \
  -H "$AUTH"
echo -e "\n"
