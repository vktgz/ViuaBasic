.function: main/1
    vpush (vpush (vector %1) (string %2 "formatted")) (string %2 "World")

    frame ^[(param %0 (string %2 "Hello, #{0} #{1}!")) (param %1 %1)]
    print (msg %3 format/)

    izero %0 local
    return
.end
