#include <Arduino.h>
#include <lcd_definitions.h>
#include <lcd.h>
#include "lcd.c"

#define F_CPU 16000000UL
#define USART_BAUDRATE 9600
#define BAUD_PRESCALE (((F_CPU / (USART_BAUDRATE * 16UL))) - 1)

volatile uint8_t x = 0;
volatile uint8_t y = 0;

float readValue(uint8_t channel) {
    ADMUX = ((ADMUX & 0xF0) | (channel & 0x0F));
    _delay_ms(20);

    ADCSRA |= (1 << ADSC);
    while (ADCSRA & (1 << ADSC));
    double val = ADCL | (ADCH << 8);
    return (val * 5) / 1024;;
}

void sendOver(char* data) {
    while (*data) {
        while (!(UCSR0A & (1 << UDRE0)));
        UDR0 = *data;
        data++;
    }
}

void read_send_data() {
    double value = readValue(0);
    uint16_t x_position = value / 5 * 1000;
    value = readValue(1);
    uint16_t y_position = value / 5 * 1000;

    char sendResult[64];
    sprintf(sendResult, "X:%d,Y:%d\n", x_position, y_position);
    sendOver(sendResult);
    _delay_ms(10);
}


int main() {

    cli();

    // Input
    DDRC &= ~((1 << PC0) | (1 << PC1));
    PORTC = 0x00; // pull-ups off

    // LCD
    // lcd_init(LCD_DISP_ON_CURSOR);

    // ADC

    ADMUX |= (1 << REFS0);
    ADCSRA |= (1 << ADEN);
    // ADCSRA |= (1 << ADIE);// should I switch to interrupt
    ADCSRA |= (1 << ADPS2) | (1 << ADPS1) | (1 << ADPS0);

    // USART

    UCSR0B |= (1 << RXEN0) | (1 << TXEN0); // Enable receiver and transmitter
    UCSR0C |= (1 << UCSZ01) | (1 << UCSZ00); // Set frame format: 8 data bits, 1 stop bit
    UBRR0L = BAUD_PRESCALE;
    UBRR0H = (BAUD_PRESCALE >> 8);

    sei();


    while (1) {
        // x is blue
        // y is white


        read_send_data();
        continue;

        lcd_clrscr();
        lcd_gotoxy(0,0);
        // lcd_puts("X");
        double value = readValue(0);
        uint16_t x_position = value / 5 * 1000;
        char buffer[16];
        sprintf(buffer, "%d", x_position);
        // lcd_puts(buffer);

        lcd_gotoxy(0, 1);
        // lcd_puts("Y");
        value = readValue(1);
        uint16_t y_position = value / 5 * 1000;
        sprintf(buffer, "%d", y_position);
        // lcd_puts(buffer);
        char sendResult[64];
        sprintf(sendResult, "X:%d,Y:%d\n", x_position, y_position);
        sendOver(sendResult);
        _delay_ms(200);
    }
    return 0;
}