typedef struct {
  float x, y, z;
} vec3;

typedef struct {
  float x, y, z, w;
} vec4;

typedef struct {
  float x, y, z, w;
} quat;

typedef struct {
  float elements[16];
} mat4;

// vec3加法
vec3 vec3_add(vec3 v1, vec3 v2) {
  vec3 result;
  result.x = v1.x + v2.x;
  result.y = v1.y + v2.y;
  result.z = v1.z + v2.z;
  return result;
}

// vec3减法
vec3 vec3_subtract(vec3 v1, vec3 v2) {
  vec3 result;
  result.x = v1.x - v2.x;
  result.y = v1.y - v2.y;
  result.z = v1.z - v2.z;
  return result;
}

// vec3长度
float vec3_length(vec3 v) { return sqrt(v.x * v.x + v.y * v.y + v.z * v.z); }

// vec3归一化
vec3 vec3_normalize(vec3 v) {
  float length = vec3_length(v);
  vec3 result;
  result.x = v.x / length;
  result.y = v.y / length;
  result.z = v.z / length;
  return result;
}

// vec4加法
vec4 vec4_add(vec4 v1, vec4 v2) {
  vec4 result;
  result.x = v1.x + v2.x;
  result.y = v1.y + v2.y;
  result.z = v1.z + v2.z;
  result.w = v1.w + v2.w;
  return result;
}

// vec4减法
vec4 vec4_subtract(vec4 v1, vec4 v2) {
  vec4 result;
  result.x = v1.x - v2.x;
  result.y = v1.y - v2.y;
  result.z = v1.z - v2.z;
  result.w = v1.w - v2.w;
  return result;
}

// quat乘法
quat quat_multiply(quat q1, quat q2) {
  quat result;
  result.x = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
  result.y = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
  result.z = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w;
  result.w = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
  return result;
}

// mat4加法
mat4 mat4_add(mat4 m1, mat4 m2) {
  mat4 result;
  for (int i = 0; i < 16; ++i) {
    result.elements[i] = m1.elements[i] + m2.elements[i];
  }
  return result;
}

// mat4乘法
mat4 mat4_multiply(mat4 m1, mat4 m2) {
  mat4 result;
  for (int i = 0; i < 4; i++) {
    for (int j = 0; j < 4; j++) {
      result.elements[i * 4 + j] = 0.0f;
      for (int k = 0; k < 4; k++) {
        result.elements[i * 4 + j] +=
            m1.elements[i * 4 + k] * m2.elements[k * 4 + j];
      }
    }
  }
  return result;
}

// mat4与float相乘
mat4 mat4_multiply_float(mat4 m, float value) {
  mat4 result;
  for (int i = 0; i < 16; ++i) {
    result.elements[i] = m.elements[i] * value;
  }
  return result;
}

// vec3与mat4相乘
vec3 vec3_multiply_mat4(vec3 v, mat4 m) {
  vec3 result;
  result.x = v.x * m.elements[0] + v.y * m.elements[4] + v.z * m.elements[8] +
             m.elements[12];
  result.y = v.x * m.elements[1] + v.y * m.elements[5] + v.z * m.elements[9] +
             m.elements[13];
  result.z = v.x * m.elements[2] + v.y * m.elements[6] + v.z * m.elements[10] +
             m.elements[14];
  return result;
}

// vec4与mat4相乘
vec4 vec4_multiply_mat4(vec4 v, mat4 m) {
  vec4 result;
  result.x = v.x * m.elements[0] + v.y * m.elements[4] + v.z * m.elements[8] +
             v.w * m.elements[12];
  result.y = v.x * m.elements[1] + v.y * m.elements[5] + v.z * m.elements[9] +
             v.w * m.elements[13];
  result.z = v.x * m.elements[2] + v.y * m.elements[6] + v.z * m.elements[10] +
             v.w * m.elements[14];
  result.w = v.x * m.elements[3] + v.y * m.elements[7] + v.z * m.elements[11] +
             v.w * m.elements[15];
  return result;
}

kernel void multiply(global const vec4 *a, global const mat4 *b,
                     global vec4 *c) {

  int i = get_global_id(0);

  c[i] = vec4_multiply_mat4(a[i], b[i]);
}