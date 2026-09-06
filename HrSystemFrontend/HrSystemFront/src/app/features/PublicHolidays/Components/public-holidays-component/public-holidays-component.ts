import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PublicHolidayFacade } from '../../../../core/Facade/public-holiday-facade';
import { Header } from '../../../../shared/components/header/header';
import { Footer } from '../../../../shared/components/footer/footer';
@Component({
  standalone: true,
  imports: [Footer, Header, CommonModule, FormsModule],
  selector: 'app-public-holidays-component',
  styleUrl: './public-holidays-component.css',
  templateUrl: './public-holidays-component.html',
})
export class PublicHolidaysComponent implements OnInit {
  readonly holidayFacade = inject(PublicHolidayFacade);

  holidayName: string = '';
  holidayDate: string = '';
  
  isEditing: boolean = false;
  editingId: string | null = null;

  ngOnInit(): void {
    this.holidayFacade.loadAll();
    this.holidayFacade.loadAllFuture();
  }

  onSave(): void {
    if (!this.holidayName || !this.holidayDate) return;

    const payload = { name: this.holidayName, date: this.holidayDate };

    if (this.isEditing && this.editingId) {
      this.holidayFacade.updateHoliday(this.editingId, payload, () => this.resetForm());
    } else {
      this.holidayFacade.createHoliday(payload, () => this.resetForm());
    }
  }

  onEdit(holiday: { id: string; name: string; date: string }): void {
    this.isEditing = true;
    this.editingId = holiday.id;
    this.holidayName = holiday.name;
    this.holidayDate = holiday.date;
  }

  onDelete(id: string): void {
    if (confirm('Are you sure you want to delete this holiday?')) {
      this.holidayFacade.deleteHoliday(id);
    }
  }

   closeAlert(){
    this.holidayFacade.errorMessage.set(null);
    this.holidayFacade.successMessage.set(null);
  }
  resetForm(): void {
    this.isEditing = false;
    this.editingId = null;
    this.holidayName = '';
    this.holidayDate = '';
  }
}
