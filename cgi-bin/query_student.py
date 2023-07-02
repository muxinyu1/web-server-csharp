#!/usr/bin/env python
import cgi
import mysql.connector
import logging
import json

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
            data = {'student_id': f'{row[0]}', 'stduent_class': f'{row[1]}', 'student_name': f'{row[2]}'}
            result = {'success': True, 'data': data}
            print(json.dumps(result))
    except Exception as e:
        print(json.dumps({'success': False, 'data': None}))
