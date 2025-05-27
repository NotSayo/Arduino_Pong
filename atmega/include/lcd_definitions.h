/*
 * lcd_definitions.h
 * Konfiguration fuer Atmega2560
 */

#ifndef LCD_LCD_DEFINITIONS_H_
#define LCD_LCD_DEFINITIONS_H_

#include <avr/io.h>

// #warning todo!
#define LCD_IO_MODE     1            /**< 0: memory mapped mode, 1: IO port mode */
#define LCD_PORT         PORTB        /**< port for the LCD lines   */
#define LCD_DATA0_PORT   LCD_PORT     /**< port for 4bit data bit 0 */
#define LCD_DATA1_PORT   LCD_PORT     /**< port for 4bit data bit 1 */
#define LCD_DATA2_PORT   LCD_PORT     /**< port for 4bit data bit 2 */
#define LCD_DATA3_PORT   LCD_PORT     /**< port for 4bit data bit 3 */
#define LCD_DATA0_PIN    PB0          /**< pin for 4bit data bit 0  */
#define LCD_DATA1_PIN    PB1          /**< pin for 4bit data bit 1  */
#define LCD_DATA2_PIN    PB2          /**< pin for 4bit data bit 2  */
#define LCD_DATA3_PIN    PB3          /**< pin for 4bit data bit 3  */
#define LCD_RS_PORT      PORTD     /**< port for RS line         */
#define LCD_RS_PIN       PD5          /**< pin  for RS line         */
#define LCD_RW_PORT      PORTD     /**< port for RW line         */
#define LCD_RW_PIN       PD6          /**< pin  for RW line         */
#define LCD_E_PORT       PORTD     /**< port for Enable line     */
#define LCD_E_PIN        PD7          /**< pin  for Enable line     */

#endif /* LCD_LCD_DEFINITIONS_H_ */
