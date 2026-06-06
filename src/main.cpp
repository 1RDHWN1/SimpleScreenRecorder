#include <QApplication>
#include <QStyle>
#include <QStyleFactory>
#include <QFont>
#include "mainwindow.h"

int main(int argc, char *argv[]) {
    QApplication app(argc, argv);

    // Set application style
    app.setStyle(QStyleFactory::create("Fusion"));

    // Set application font
    QFont font("Segoe UI", 10);
    app.setFont(font);

    // Create and show main window
    MainWindow window;
    window.show();

    return app.exec();
}
