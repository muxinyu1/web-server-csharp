#!/usr/bin/env python
import cgi
import mysql.connector
import logging

form = cgi.FieldStorage()
if form.getvalue('student_id') and form.getvalue('student_class') and form.getvalue('student_name'):
    student_id = str(form.getvalue('student_id'))
    student_class = str(form.getvalue('student_class'))
    student_name = str(form.getvalue('student_name'))
    try:
        db = mysql.connector.connect(host='mysql', username='root', password='muxinyu1')
        cursor = db.cursor()
        cursor.execute('CREATE DATABASE IF NOT EXISTS student_db')
        cursor.execute('USE student_db')

        cursor.execute("SHOW TABLES LIKE 'student'")
        result = cursor.fetchone()
        if not result:
            cursor.execute('CREATE TABLE student (id VARCHAR(64) PRIMARY KEY, class VARCHAR(128), name VARCHAR(128))')
        
        sql = 'INSERT INTO student (id, class, name) VALUES (%s, %s, %s)'
        val = (student_id, student_class, student_name)
        cursor.execute(sql, val)

        db.commit()

        print("success: {}".format(cursor.lastrowid))
    except Exception as e:
        logging.exception(e)