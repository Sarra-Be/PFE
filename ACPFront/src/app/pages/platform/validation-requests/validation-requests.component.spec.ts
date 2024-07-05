import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidationRequestsComponent } from './validation-requests.component';

describe('ValidationRequestsComponent', () => {
  let component: ValidationRequestsComponent;
  let fixture: ComponentFixture<ValidationRequestsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidationRequestsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ValidationRequestsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
