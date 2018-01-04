.function: main/0
  fstore %2 local 1.4
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  fstore %2 local 1.5
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  fstore %2 local 1.6
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  fstore %2 local -1.4
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  fstore %2 local -1.5
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  fstore %2 local -1.6
  ftoi %3 local %2 local
  frame ^[(param %0 %2 local)]
  call %4 local round/1
  print %2 local
  print %3 local
  print %4 local
  izero %0 local
  return
.end

.function: round/1
  arg %1 local %0
  if (lt %3 local %1 local (fstore %2 local 0)) round_negative
  fstore %2 local 0.5
  jump math_round
  .mark: round_negative
  fstore %2 local -0.5
  .mark: math_round
  add %1 local %1 local %2 local
  ftoi %0 local %1 local
  return
.end
