// payload kako dolazi iz JWT-a
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
