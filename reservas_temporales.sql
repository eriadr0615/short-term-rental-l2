

DROP DATABASE IF EXISTS reservas_temporales;
CREATE DATABASE reservas_temporales
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE reservas_temporales;



CREATE TABLE Propietario (
    id_propietario INT AUTO_INCREMENT PRIMARY KEY,
    dni VARCHAR(15) NOT NULL UNIQUE,
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    telefono VARCHAR(30),
    correo VARCHAR(100),
    direccion VARCHAR(150)
);

CREATE TABLE Inquilino (
    id_inquilino INT AUTO_INCREMENT PRIMARY KEY,
    dni VARCHAR(15) NOT NULL UNIQUE,
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    telefono VARCHAR(30),
    correo VARCHAR(100),
    direccion VARCHAR(150)
);

CREATE TABLE TipoInmueble (
    id_tipo_inmueble INT AUTO_INCREMENT PRIMARY KEY,
    nombre_tipo VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Usuario (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    avatar VARCHAR(255),
    nombre_usuario VARCHAR(100) NOT NULL,
    correo_usuario VARCHAR(100) NOT NULL UNIQUE,
    contrasenia_hash VARCHAR(255) NOT NULL,
    rol_usuario VARCHAR(20) NOT NULL,
    ultima_conexion DATETIME NULL
);

CREATE TABLE Inmueble (
    id_inmueble INT AUTO_INCREMENT PRIMARY KEY,
    id_propietario INT NOT NULL,
    direccion_inmueble VARCHAR(150) NOT NULL,
    id_tipo_inmueble INT NOT NULL,
    coordenadas_inmuebles VARCHAR(100),
    precio_diario DECIMAL(12,2) NOT NULL,
    porcentaje_reserva DECIMAL(5,2) NOT NULL,
    disponible BOOLEAN NOT NULL DEFAULT TRUE,
    capacidad_maxima INT NOT NULL,

    CONSTRAINT fk_inmueble_propietario
        FOREIGN KEY (id_propietario)
        REFERENCES Propietario(id_propietario)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_inmueble_tipo
        FOREIGN KEY (id_tipo_inmueble)
        REFERENCES TipoInmueble(id_tipo_inmueble)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE TABLE Imagen_Inmueble (
    id_imagen INT AUTO_INCREMENT PRIMARY KEY,
    id_inmueble INT NOT NULL,
    url_img VARCHAR(255) NOT NULL,
    es_principal BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_imagen_inmueble
        FOREIGN KEY (id_inmueble)
        REFERENCES Inmueble(id_inmueble)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE Reserva (
    id_reserva INT AUTO_INCREMENT PRIMARY KEY,
    id_inquilino INT NOT NULL,
    id_inmueble INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin_original DATE NOT NULL,
    monto_dia DECIMAL(12,2) NOT NULL,
    fecha_finalizacion_anticipada DATE NULL,
    id_usuario_creacion INT NULL,
    id_usuario_finalizacion INT NULL,
    id_reserva_origen INT NULL,

    CONSTRAINT fk_reserva_inquilino
        FOREIGN KEY (id_inquilino)
        REFERENCES Inquilino(id_inquilino)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_reserva_inmueble
        FOREIGN KEY (id_inmueble)
        REFERENCES Inmueble(id_inmueble)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_reserva_usuario_creacion
        FOREIGN KEY (id_usuario_creacion)
        REFERENCES Usuario(id_usuario)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_reserva_usuario_finalizacion
        FOREIGN KEY (id_usuario_finalizacion)
        REFERENCES Usuario(id_usuario)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_reserva_origen
        FOREIGN KEY (id_reserva_origen)
        REFERENCES Reserva(id_reserva)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE TABLE Pago (
    id_pago INT AUTO_INCREMENT PRIMARY KEY,
    id_reserva INT NOT NULL,
    concepto VARCHAR(150) NOT NULL,
    fecha_pago DATETIME NOT NULL,
    monto DECIMAL(12,2) NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    id_usuario_creacion INT NOT NULL,
    id_usuario_anulacion INT NULL,

    CONSTRAINT fk_pago_reserva
        FOREIGN KEY (id_reserva)
        REFERENCES Reserva(id_reserva)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_pago_usuario_creacion
        FOREIGN KEY (id_usuario_creacion)
        REFERENCES Usuario(id_usuario)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT fk_pago_usuario_anulacion
        FOREIGN KEY (id_usuario_anulacion)
        REFERENCES Usuario(id_usuario)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);




INSERT INTO TipoInmueble (nombre_tipo) VALUES
('Casa'),
('Departamento'),
('Monoambiente'),
('Loft');


INSERT INTO Propietario
(dni, nombre, apellido, telefono, correo, direccion)
VALUES
('30111222', 'Carlos', 'Gomez', '2664000001', 'carlos.gomez@ejemplo.com', 'San Luis'),
('32111333', 'Laura', 'Martinez', '2664000002', 'laura.martinez@ejemplo.com', 'Villa Mercedes');

INSERT INTO Inquilino
(dni, nombre, apellido, telefono, correo, direccion)
VALUES
('35111444', 'Martin', 'Lopez', '2664000003', 'martin.lopez@ejemplo.com', 'San Luis'),
('37111555', 'Sofia', 'Fernandez', '2664000004', 'sofia.fernandez@ejemplo.com', 'Juana Koslay');



SELECT * FROM Propietario;
SELECT * FROM Inquilino;
SELECT * FROM TipoInmueble;
