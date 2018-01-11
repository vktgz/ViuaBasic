.function: main/0
  float %2 local 7.6
  frame ^[(param %0 %2 local)]
  call %1 local exp/1
  echo (string %4 local "exp(7.6) = 1998.19589510 : ")
  print %1 local
  izero %0 local
  return
.end

.function: exp/1
  .name: %0 result
  .name: %1 argum
  .name: %2 iks
  .name: %3 silnia
  .name: %4 i
  .name: %5 tmp_float
  .name: %6 tmp_bool
  .name: %7 i_max
  arg %argum %0
  float %result 1
  copy %iks %argum
  float %silnia 1
  integer %i 1
  integer %i_max 50
  .mark: exp_loop
  div %tmp_float %iks %silnia
  add %result %result %tmp_float
  echo %i
  echo (string %8 local " ")
  print %result
  mul %iks %iks %argum
  iinc %i
  mul %silnia %silnia %i
  if (lte %tmp_bool %i %i_max) exp_loop
  return
.end
