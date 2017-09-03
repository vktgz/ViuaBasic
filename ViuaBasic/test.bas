10 print "commodore   atari"
20 list
30 goto 10

5 let a:integer=0
15 let a=a+1
25 if a>3 then 35
35 print "a=",a

40 for i=2 to 8 step 2
45 print "iter : ",i
50 next i

56 let a = 3
57 let i = 8

60 if a>3 then
61  if i>8 then
62   print "a>3,i>8"
63  else
64   print "a>3,i<=8"
65  endif
66 else
67  if i>8 then
68   print "a<=3,i>8"
69  else
70   print "a<=3,i<=8"
71  endif
72 endif

80 print "7 mod 4 = 3 : ",7%4
81 print "-7 mod 4 = 1 : ",-7%4
82 print "7 mod -4 = -1 : ",7%-4
83 print "-7 mod -4 = -3 : ",-7%-4
84 print "-7 mod 4.2 = 1.4 : ",-7%4.2
85 print "-7.6 mod 4 = 0.4 : ",-7.6%4
86 print "-7.6 mod 4.2 = 0.8 : ",-7.6%4.2

90 print "2.2*3.3=",2.2*3.3
91 print "7.6 ^ 4.2 = 5005.14988637 : ",7.6^4.2
92 print "ABS(-8.2)=",abs(-8.2)
93 print "EXP(7.6) = 1998.19589510 : ",exp(7.6)
94 print "LOG(7.6) = 2.02814825 : ",log(7.6)
95 print "2 * EXP(7) - 3 = 2190.26631685 : ",2 * exp(7) - 3

100 let a = 4
101 let b:float = 3.3
102 let c:float = a*b
103 print "c=",c," or c=",a*b
104 print """escaped"""
105 print "string with ""escaped"" element"

110 print exp ( 1 * ( 2 - 1 ) + 4 / 4 )
111 dim ar(2,3) integer = 2
112 print ar ( 1 * ( 2 - 1 ) , 4 / 4 )
113 dim ar2(2,3,4) integer = 6
114 print ar2(0,2,ar(1,1))
115 print ar2(0,ar(1,1),3)
116 if ar2(0,ar(1,1),3) = 6 then
117 print "ok"
118 endif
119 let ar ( 1 , 1 ) = 4
120 print ar(1,1)
121 let z:string = ""
122 for x = 0 to 1
123   if x = 0 then
124     let z = z,"["
125   else
126     let z = z,","
127   endif
128   for y = 0 to 2
129     if y = 0 then
130       let z = z,"["
131     else
132       let z = z,","
133     endif
134     let z = z,ar(x,y)
135   next y
136   let z = z,"]"
137 next x
138 let z = z,"]"
139 print z
