#!/usr/bin/env python
import cgi

form = cgi.FieldStorage()
if form.getvalue('num1') and form.getvalue('num2'):
    num1 = int(form.getvalue('num1'))
    num2 = int(form.getvalue('num2'))
    sum = num1 + num2
    with open("./webroot/sum.html", "r") as f:
        html = f.read()
        html = html.replace("mxy2233@result.com", "The sum of {} and {} is {}.".format(num1, num2, sum))
        print(html)
