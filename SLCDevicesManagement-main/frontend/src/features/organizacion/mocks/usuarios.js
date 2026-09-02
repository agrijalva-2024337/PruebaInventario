import { usuariosSesion } from '@/features/auth/mocks/usuariosSesion';

export const usuarios = usuariosSesion.map((usuario) => ({ ...usuario }));
