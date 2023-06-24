#!/usr/bin/env python
import cgi

form = cgi.FieldStorage()
if form.getvalue('num1') and form.getvalue('num2'):
    num1 = int(form.getvalue('num1'))
    num2 = int(form.getvalue('num2'))
    sum = num1 + num2
    print("<html><body>")
    print("The sum of {} and {} is {}".format(num1, num2, sum))
    print("</body></html>")