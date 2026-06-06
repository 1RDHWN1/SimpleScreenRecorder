#ifndef REGIONSELECTIONDIALOG_H
#define REGIONSELECTIONDIALOG_H

#include <QDialog>
#include <QRect>
#include <QPoint>

class RegionSelectionDialog : public QDialog {
    Q_OBJECT

public:
    explicit RegionSelectionDialog(QWidget *parent = nullptr);
    ~RegionSelectionDialog();

    QRect getSelectedRegion() const { return selectedRegion; }

protected:
    void mousePressEvent(QMouseEvent *event) override;
    void mouseMoveEvent(QMouseEvent *event) override;
    void mouseReleaseEvent(QMouseEvent *event) override;
    void paintEvent(QPaintEvent *event) override;
    void keyPressEvent(QKeyEvent *event) override;

private:
    void drawSelection();

    QRect selectedRegion;
    QPoint startPoint;
    QPoint endPoint;
    bool isSelecting = false;
};

#endif // REGIONSELECTIONDIALOG_H
