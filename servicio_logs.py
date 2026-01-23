import csv
import os
from datetime import datetime

# Nombre del archivo log
ARCHIVO_LOG = "registro_eventos.csv"

def limpiar_pantalla():
    os.system('cls' if os.name == 'nt' else 'clear')

def mostrar_menu():
    print("\n--- SERVICIO DE LOGS: BARRIO INTELIGENTE ---")
    print("1. Ingresar nuevo evento")
    print("2. Mostrar historial de eventos")
    print("3. Reiniciar/Sobrescribir logs")
    print("4. Salir")
    return input("Seleccione una opción: ")

def ingresar_datos():
    evento = input("Describa el evento o error reportado: ")
    usuario = input("Usuario que reporta (ID o Nombre): ")
    fecha = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    # Validar que no estén vacíos
    if not evento or not usuario:
        print("Error: Los datos no pueden estar vacíos.")
        return

    # Escribir en el archivo
    existe = os.path.isfile(ARCHIVO_LOG)
    
    try:
        with open(ARCHIVO_LOG, mode='a', newline='', encoding='utf-8') as file:
            writer = csv.writer(file)
            # Si es nuevo, escribimos cabeceras
            if not existe:
                writer.writerow(["FECHA", "USUARIO", "EVENTO"])
            
            writer.writerow([fecha, usuario, evento])
        print(" Evento registrado exitosamente.")
    except Exception as e:
        print(f"Error al escribir en el archivo: {e}")

def leer_logs():
    # Validar si existe datos
    if not os.path.isfile(ARCHIVO_LOG):
        print(" No hay registros de logs actualmente.")
        return

    print("\n--- HISTORIAL DE EVENTOS ---")
    try:
        with open(ARCHIVO_LOG, mode='r', encoding='utf-8') as file:
            reader = csv.reader(file)
            datos = list(reader)
            
            if len(datos) <= 1: # Solo cabeceras o vacío
                print(" El archivo existe pero está vacío.")
            else:
                for row in datos:
                    print(f"[{row[0]}] Usuario: {row[1]} | Evento: {row[2]}")
    except Exception as e:
        print(f"Error al leer el archivo: {e}")

def sobrescribir_logs():
    confirmacion = input("¿Está seguro que desea BORRAR todo el historial? (s/n): ")
    if confirmacion.lower() == 's':
        try:
            # Mode 'w' sobrescribe y deja el archivo en blanco
            with open(ARCHIVO_LOG, mode='w', newline='', encoding='utf-8') as file:
                writer = csv.writer(file)
                writer.writerow(["FECHA", "USUARIO", "EVENTO"])
            print("Logs reiniciados correctamente.")
        except Exception as e:
            print(f"Error al reiniciar logs: {e}")

#Ejecución Principal
def main():
    while True:
        opcion = mostrar_menu()
        
        if opcion == '1':
            ingresar_datos()
        elif opcion == '2':
            leer_logs()
        elif opcion == '3':
            sobrescribir_logs()
        elif opcion == '4':
            print("Cerrando servicio...")
            break
        else:
            print("Opción no válida.")
        
        input("\nPresione Enter para continuar...")
        limpiar_pantalla()

if __name__ == "__main__":
    main()