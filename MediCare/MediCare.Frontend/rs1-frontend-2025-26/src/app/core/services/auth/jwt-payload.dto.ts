// payload as it comes from the JWT
export interface JwtPayloadDto {
  sub: string;
  email: string;
  roleId:number,
  roleName:string,
  ver: string;
  iat: number;
  exp: number;
  aud: string;
  iss: string;
}
