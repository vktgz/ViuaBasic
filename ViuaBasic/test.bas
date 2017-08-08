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
