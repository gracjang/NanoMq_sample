docker run -d -p 1883:1883 -p 8883:8883 \
           -e NANOMQ_BROKER_URL="nmq-tcp://0.0.0.0:1883" \
           -e NANOMQ_TLS_ENABLE=true \
           -e NANOMQ_TLS_URL="tls+nmq-tcp://0.0.0.0:8883" \
           --name nanomq emqx/nanomq:latest