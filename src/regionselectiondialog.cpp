#include "regionselectiondialog.h"
#include <QScreen>
#include <QGuiApplication>
#include <QMouseEvent>
#include <QPainter>
#include <QKeyEvent>
#include <QLabel>
#include <QVBoxLayout>

RegionSelectionDialog::RegionSelectionDialog(QWidget *parent)
    : QDialog(parent) {
    
    setWindowTitle("Select Recording Region");
    setWindowFlags(Qt::FramelessWindowHint | Qt::WindowStaysOnTopHint);
    setStyleSheet("background-color: rgba(0, 0, 0, 0.5);");
    setMouseTracking(true);
    setCursor(Qt::CrossCursor);

    // Set to fullscreen
    QScreen *screen = QGuiApplication::primaryScreen();
    setGeometry(screen->geometry());

    setFocus();
}

RegionSelectionDialog::~RegionSelectionDialog() = default;

void RegionSelectionDialog::mousePressEvent(QMouseEvent *event) {
    startPoint = event->globalPosition().toPoint();
    endPoint = startPoint;
    isSelecting = true;
    drawSelection();
}

void RegionSelectionDialog::mouseMoveEvent(QMouseEvent *event) {
    if (isSelecting) {
        endPoint = event->globalPosition().toPoint();
        update();
    }
}

void RegionSelectionDialog::mouseReleaseEvent(QMouseEvent *event) {
    if (isSelecting) {
        endPoint = event->globalPosition().toPoint();
        isSelecting = false;

        // Normalize rectangle
        int x = qMin(startPoint.x(), endPoint.x());
        int y = qMin(startPoint.y(), endPoint.y());
        int w = qAbs(endPoint.x() - startPoint.x());
        int h = qAbs(endPoint.y() - startPoint.y());

        selectedRegion = QRect(x, y, w, h);

        if (selectedRegion.width() > 0 && selectedRegion.height() > 0) {
            accept();
        }
    }
}

void RegionSelectionDialog::paintEvent(QPaintEvent *event) {
    QPainter painter(this);

    if (isSelecting) {
        // Draw selection rectangle
        int x = qMin(startPoint.x(), endPoint.x());
        int y = qMin(startPoint.y(), endPoint.y());
        int w = qAbs(endPoint.x() - startPoint.x());
        int h = qAbs(endPoint.y() - startPoint.y());

        // Draw filled rectangle with semi-transparent color
        painter.fillRect(x, y, w, h, QColor(0, 150, 255, 100));

        // Draw border
        painter.setPen(QPen(Qt::cyan, 2));
        painter.drawRect(x, y, w, h);

        // Draw size text
        QString sizeText = QString("%1x%2").arg(w).arg(h);
        painter.setPen(Qt::white);
        painter.setFont(QFont("Arial", 12, QFont::Bold));
        painter.drawText(x + 5, y + 25, sizeText);
    }

    // Draw help text
    painter.setPen(Qt::white);
    painter.setFont(QFont("Arial", 14));
    painter.drawText(10, 30, "Click and drag to select recording region | Press ESC to cancel");
}

void RegionSelectionDialog::keyPressEvent(QKeyEvent *event) {
    if (event->key() == Qt::Key_Escape) {
        selectedRegion = QRect();
        reject();
    }
}
