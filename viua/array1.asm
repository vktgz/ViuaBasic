.function: main/0
  vector %1 local
  vpush %1 local (integer %2 local 2)
  vpush %1 local (integer %2 local 3)
  integer %2 local 5
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %1 local array_create/2
  print (string %4 local "dim(2,3)integer = 5")
  print %1 local

  vector %2 local
  vpush %2 local (integer %3 local 0)
  vpush %2 local (integer %3 local 1)
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %3 local array_get/2
  echo (string %4 local "get(0,1) = ")
  print %3 local
  print %1 local

  ptr %5 local %1 local
  integer %6 local 8
  frame ^[(param %0 %5 local) (param %1 %2 local) (param %2 %6 local)]
  call %3 local array_set/3
  print (string %4 local "set(0,1) = 8")
  print %1 local

  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %3 local array_get/2
  echo (string %4 local "get(0,1) = ")
  print %3 local
  print %1 local

  vector %1 local
  vpush %1 local (integer %2 local 7)
  float %2 local 7.4
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %1 local array_create/2
  print (string %4 local "dim(7)float = 7.4")
  print %1 local
  izero %0 local
  return
.end

.function: array_create/2
  arg %1 local %0
  arg %2 local %1
  vector %0 local
  integer %5 local 0
  if (gt %3 local (vlen %4 local %1 local) %5 local) ar_dims
  throw (string %6 local "array dimension must be greater than zero")
  .mark: ar_dims
  vpop %4 local %1 local %5 local
  if (gt %3 local %4 local %5 local) ar_dim
  throw (string %6 local "array dimension must be greater than zero")
  .mark: ar_dim
  if (eq %3 local (vlen %7 local %1 local) %5 local) ar_fill_val
  .mark: ar_fill_arr
  if (eq %3 local %4 local %5 local) ar_done
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %7 local array_create/2
  vpush %0 local %7 local
  idec %4 local
  jump ar_fill_arr
  .mark: ar_fill_val
  if (eq %3 local %4 local %5 local) ar_done
  copy %7 local %2 local
  vpush %0 local %7 local
  idec %4 local
  jump ar_fill_val
  .mark: ar_done
  return
.end

.function: array_get/2
  arg %1 local %0
  arg %2 local %1
  integer %5 local 0
  if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims
  throw (string %6 local "array dimension do not match")
  .mark: ar_dims
  vpop %4 local %2 local %5 local
  if (gte %3 local %4 local %5 local) ar_bound
  if (lt %3 local %4 local (vlen %7 local %1 local)) ar_bound
  throw (string %6 local "array index out of bounds")
  .mark: ar_bound
  vpop %0 local %1 local %4 local
  if (eq %3 local (vlen %7 local %2 local) %5 local) ar_done
  frame ^[(param %0 %0 local) (param %1 %2 local)]
  call %0 local array_get/2
  .mark: ar_done
  return
.end

.function: array_set/3
  arg %1 local %0
  arg %2 local %1
  arg %8 local %2
  integer %5 local 0
  if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims
  throw (string %6 local "array dimension do not match")
  .mark: ar_dims
  vpop %4 local %2 local %5 local
  if (gte %3 local %4 local %5 local) ar_bound
  if (lt %3 local %4 local (vlen %7 local *1 local)) ar_bound
  throw (string %6 local "array index out of bounds")
  .mark: ar_bound
  if (eq %3 local (vlen %7 local %2 local) %5 local) ar_set
  vat %0 local *1 local %4 local
  frame ^[(param %0 %0 local) (param %1 %2 local) (param %2 %8 local)]
  call %0 local array_set/3
  return
  .mark: ar_set
  vpop %0 local *1 local %4 local
  vinsert *1 local %8 local %4 local
  return
.end
