#!/usr/bin/env python
import cgi
import mysql.connector
import logging

form = cgi.FieldStorage()
if form.getvalue('student_id'):
    student_id = form.getvalue('student_id')
    try:
        db = mysql.connector.connect(host='localhost', database='student_db', username='root', password='muxinyu1')
        cursor = db.cursor()
        
        sql = 'SELECT * FROM student WHERE id = %s'
        val = (student_id, )

        cursor.execute(sql, val)

        result = cursor.fetchall()
        
        for row in result:
            print(row)
    except Exception as e:
        logging.exception(e)
