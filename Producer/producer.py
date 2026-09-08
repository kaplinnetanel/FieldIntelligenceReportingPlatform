from confluent_kafka import Producer
import socket
import os
import json
from logger import get_logger

log = get_logger(__name__)

def producer():
    try :
        producer = Producer({
    "bootstrap.servers": os.getenv(
        "KAFKA_BOOTSTRAP_SERVERS",
        "localhost:9092"
        )
    })
        try: 
            with open ("field_reports.json",encoding="utf-8") as f:
                data = json.load(f)
                log.info("open file the field_reports ")
                topic = "topic"
                c =  0 
                for row in data:
                    producer.produce(topic,value=json.dumps(row))
                    print(row)
                    c += 1
                    log.info("the masseg send to kafka")
                producer.flush()
                print("total to send kafka = ",c) 
        except Exception as e:
            print(f"eror in open file field_reports {e}")
    except Exception as s :
        log.error("eror in the producer")
        print(f"eror in the producer {s}")
if __name__ == "__main__":
  producer()

