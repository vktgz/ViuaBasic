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

2 print """escaped"""
3 print "string with ""escaped"" element"
