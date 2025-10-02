import string

from woocommerce import API
import argparse
import json
import sys
from urllib import parse



# root "https://spica.hal.si/" "ck_f2cc094e7332795dd38422e305f9ca07bff35823" "cs_9e2930c7310febea5a69c165345f3da04bbb2b32" "wc/v3"
# get test: += "get" "products/"
# create product test: += "post" "products" --bodyfile "C:/Users/km/PycharmProjects/woocommerce/some.txt"
#
def argsparser():
    parser = argparse.ArgumentParser(description='Call the woocommerce API')
    parser.add_argument('address', type=str, help='the address of the wordpress site')
    parser.add_argument('ck', type=str, help='woocommerce consumer key')
    parser.add_argument('cs', type=str, help='woocommerce consumer secret')
    parser.add_argument('version', type=str, help='woocommerce api version')
    parser.add_argument('http_op', type=str, help='type of http request')
    parser.add_argument('op', type=str, help='woocommerce api operation to execute')
    parser.add_argument('--bodyfile', type=str, help='the file where the body of the woocommerce request is kept')

    args = parser.parse_args()
    return args

def main(args):
    sys.stdout.reconfigure(encoding='utf-8')
    wcapi = API(
        url=args.address,
        consumer_key=args.ck,
        consumer_secret=args.cs,
        wp_api=True,
        version=args.version,
        query_string_auth=True,
        timeout=50  # Dodam timeout, ker večji klici lahko dlje trajajo, ni specifika REST API ampak Python modula
    )

    
    res = ''

    if 'my_' not in args.op:
        if '?' in args.op:
            parsedparams = dict(parse.parse_qsl(parse.urlsplit(args.op).query))
            args.op = args.op[0:args.op.index('?')]
        else:
            parsedparams = dict()
        if (args.http_op == 'get'):
            res = wcapi.get(args.op, params=parsedparams).text
        elif (args.http_op == 'post'):
            jsonstr = open(args.bodyfile, 'r', encoding='utf-8').read()
            data = json.loads(jsonstr)
            res = wcapi.post(args.op, data, params=parsedparams).text
        elif (args.http_op == 'put'):
            jsonstr = open(args.bodyfile, 'r', encoding='utf-8').read()
            data = json.loads(jsonstr)
            res = wcapi.put(args.op, data, params=parsedparams).text
        elif (args.http_op == 'delete'):
            res = wcapi.delete(args.op, params=parsedparams)
    else:
        if (args.http_op == 'get'):
            res = wcapi.get(args.op).text
        elif (args.http_op == 'post'):
            jsonstr = open(args.bodyfile, 'r', encoding='utf-8').read()
            data = json.loads(jsonstr)
            res = wcapi.post(args.op, data).text
        elif (args.http_op == 'put'):
            jsonstr = open(args.bodyfile, 'r', encoding='utf-8').read()
            data = json.loads(jsonstr)
            res = wcapi.put(args.op, data).text
        elif (args.http_op == 'delete'):
            res = wcapi.delete(args.op)
    print(res)


if __name__ == '__main__':
    args = argsparser()
    main(args)
