.function: main/0
  frame ^[(param %0 (self %1 local))]
  process %2 global seeder/1
  izero %2 local
  integer %3 local 1000
  .mark: wait_loop
  iinc %2 local
  echo (string %6 local "wait loop = ")
  print %2 local
  if (lt %4 local %2 local %3 local) wait_loop
  send %2 global %2 local
  receive %5 local infinity
  echo (string %6 local "seed = ")
  print %5 local
  izero %0 local
  return
.end

.function: seeder/1
  arg %1 local %0
  izero %2 local
  izero %3 local
  string %4 local "doing nothing"
  izero %5 local
  .mark: seed_loop
  iinc %2 local
  try
  catch "Exception" .block: do_nothing
    print %4 local
    leave
  .end
  enter .block: wait_msg
    receive %3 local 1ms
    leave
  .end
  if (eq %6 local %3 local %5 local) seed_loop
  send %1 local %2 local
  izero %0 local
  return
.end
