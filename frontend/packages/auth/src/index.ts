export { tokens, obtenerAccessTokenVigente, API_BASE_URL, ApiError, mensajeDeError, describirConcurrencia } from './api';
export { avisarError } from './conflictos';
export {
  INICIO_POR_ROL,
  ETIQUETA_ROL,
  ROLES_SERVICIO,
  inicioDeRol,
  etiquetaRol,
  esRolServicio,
  urlServicio,
  urlAdmin,
} from './roles';
export { AuthProvider, useAuth } from './AuthContext';
export type { AuthValue, UsuarioSesion } from './AuthContext';
export { LoginPage } from './LoginPage';
export type { LoginPageProps, AplicacionLogin } from './LoginPage';