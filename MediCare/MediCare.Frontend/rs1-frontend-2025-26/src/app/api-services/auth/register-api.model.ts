export interface RegisterCommand{
    firstName:string,
    lastName:string,
    dateOfBirth:string,
    address:string,
    city:string,
    userName:string,
    email:string,
    password:string,
    phoneNumber:string
}

export interface RegisterCommandDto{
    userName:string,
    email:string
}